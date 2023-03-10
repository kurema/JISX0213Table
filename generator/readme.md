# generatorについて
このフォルダはJIS X 0213のUnicodeテーブルを生成するコードです。
現在までに5度試みられています。
最新のものは[Jis2UCS4](Jis2UCS4)です

## Jis2UCS
YamlをC#のswitchに変更するシンプルなコードです。
読み込み処理を省略でき、C#のswitchはジャンプテーブルで速いそうなのでコードにした方が良いかと考えました。
しかしコンパイルに時間が掛かり、開くとエディタが重くなり、バイナリのサイズが0.9MBにもなりました(Debugビルド)。

変換は簡単にPerlで行っています。

```csharp
case "1-01-01": return "&#x3000;";
```

## Jis2UCS2 - 実体参照版
`Jis2UCS`ではswitchのcaseはstringで例えば`case "01-01-01":`のような形式にしていましたが、多段switchでintとして扱った方が確実だと考えました。
その結果、バイナリのサイズは0.9MBから0.4MBまで減りました。

以降変換はC#で行っています。

```csharp
switch (num1)
{
    case 1:
        switch (num2)
        {
            case 1:
            switch (num3)
            {
                case 1: return "&#x3000;";
            }
        }
}
```

## Jis2UCS2 - ユニコード版
これ以前は対応するユニコードの文字(列)ではなく実体参照での表現を返していましたが、ここで文字列を返すようにしました。
当然そうすべきなのですが問題があります。

* 本来通りの実体参照が必要。何らかのUnicode絡みの処理で実体参照に変換した結果が期待と違うと困る。
* 扱いやすいXMLエスケープ処理機能が見当たらなかった。
* その場合は処理に`EnumerateRunes()`が欲しかったが、.NET Standard 2.0に無かった(実際は必要なかったと思う)。

なおバイナリサイズはこの時点で234KBです。

```csharp
switch (num1)
{
    case 1:
        switch (num2)
        {
            case 1:
            switch (num3)
            {
                case 1: return "\u3000"; // :1-01-01: "&#x3000;"
            }
        }
}
```

## Jis2UCS3
この辺りでそもそも容量が大きすぎるし、`switch`が速いというのも胡散臭い気がしてきました。
素直にバイナリ形式にすれば良いのではと考え、そうしました。
バイナリは「埋め込みリソース」を使えば簡単に参照できます。

ここでは単純にほぼUTF-16形式にして、4バイト(対応するユニコード表現が2文字の場合があるので)の3次元配列にしています。
厳密には先頭4バイトをUTF-16のBOMにしているのでそれだけずれます。
また実体参照を復元するのが目的なのでサロゲートペアの1バイト目は00 02にしています。

この時点で70KBです。
十分小さいですが、バイナリエディタで見ると空白が多過ぎるのが気になりました。
対処としてgzip形式で埋め込み初回読み込み時に解凍していました。
gzip形式では22KBです。

## Jis2UCS4
70KBでも十分小さくこれ以上やる意味はないと思いましたが、せっかくなのでもう少しコンパクトにしました。

まず基本を2バイト単位に変更しました。ビット単位での処理をするつもりはないのでそれ以下にはなりません。
そのままだとサロゲートペアや2文字の場合が収まらないので、その場合は別の場所にジャンプすることにしました。
ジャンプ先は本来のファイル終端より後でも良いのですが、隙間が多いのでそこを使おうと考えました。
検索したところ、コードポイントがA～Eで始まる文字はなかったので、A始まりを無効な文字、B～Eはジャンプ先指定で使うことにしました。
2文字の場合は6バイト、サロゲートペアは4バイトで表現できます。

この時点で35KBです。

さらにバイナリエディタ上では中央に大きな空白があったので、そこを省略することにしました。
参照前にアドレス変換をするだけで可能です。
これで24KBになりました。ほぼ圧縮後のサイズです。
色々な工夫をすればまだまだ縮まると思いますが、実用上の意味はないです。

最後にバイナリエディタで検索すると`00 00 00 00`が2箇所程ありました。
4バイト余った場合はサロゲートペア(4バイト)が使われるように修正をして、数バイト削減しました。

gzip圧縮後よりかはサイズが大きいですが、Byte配列のまま読み込んで処理できるのでメモリ消費量も速度も向上しているはずです。
ただ現代においてKB単位の削減は特に意味がないと考えられます。
70KBのgzip伸長に掛かる時間は1ミリ秒前後のようです。

しかしここで考えると入出力文字列のまま単純なswitchで実装して0.9MBという最初のアプローチは酷いです。
たった数か月前の話なのですが。

### 容量削減案
サロゲートペアの要素は303、2文字の要素は25です。
第二面においては文字の存在しない区があるので12バイト使用すれば空白領域を簡単に省略できます。
また2文字では2文字目は5種類しかありません。
94点存在しない面は面の末尾にA? ??で点の数を示せばサロゲートペアは2～3バイト、2文字は3バイト(2バイト+3ビット)で表現可能です。
そう考えればさらに660B程度は削減できるはずです。

ただしメモリ参照が増えてコードが複雑になります。
前者はともかく、手間は容量の割には合わないでしょう。今時1KB節約する為にコードを書く人は居ません。

ファイル内ポインタも工夫の余地があります。
サロゲートペアか2文字かのような情報を埋め込むこともできたはずです。1ビットくらいは余ります。
ただこれも割と面倒です。

頑張ってもせいぜいgzip圧縮結果と同じ22KB程度じゃないでしょうか。
なお7zipだと16KBになりますが、さすがにここまで展開処理不要のまま工夫して削減するのは無理でしょう。
