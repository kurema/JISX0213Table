# JISX0213Table
JIS X 0213のUnicodeコードポイント変換テーブルです。
本来は70KB程度必要なところ、圧縮無しで24KBに収まっています。
ただし読み取りには多少複雑な処理が必要です。

| ファイル名 | 説明 | リンク | 
| -- | -- | -- |
| jis2ucs.bin | ファイルサイズを縮小したテーブル | [詳細](./jis2ucs.bin.md) |
| other/jis2ucs.old.bin | コードポイントによる2\*94\*94のテーブル | [詳細](other/jis2ucs.old.bin.md) |
| other/jis2ucs.Simple.bin | UTF-16による2\*94\*94のテーブル | [詳細](other/jis2ucs.Simple.bin.md) |
| other/jis2ucs.Simple.Space.bin | 上のヌル文字を空白に置き換えたもの | [詳細](other/jis2ucs.Simple.bin.md#jis2ucssimplespacebin) |
| generator/ | ファイル生成プログラム | [詳細](generator/readme.md) |
| samples/CSharp/ | 参照用ソースコードのサンプル | |

4種類用意しましたのでお好みのファイルをご利用ください。
ファイル形式について詳しくは上の「詳細」から参照してください。

## 概要
このレポジトリは[aozora2htmlSharp](https://github.com/kurema/aozora2htmlSharp)の派生です。

本ファイルは[aozorahack](https://github.com/aozorahack)/[aozora2html](https://github.com/aozorahack/aozora2html)の[jis2ucs.yml](https://github.com/aozorahack/aozora2html/blob/master/yml/jis2ucs.yml)から作成されています。
jis2ucs.ymlは[JIS X 0213:2004 vs Unicode mapping table](http://w3.kcua.ac.jp/~fujiwara/jis2000/jis2004/jisx0213-2004-mono.html)([Archive](https://web.archive.org/web/20160314032417/http://w3.kcua.ac.jp/~fujiwara/jis2000/jis2004/jisx0213-2004-mono.html))を元に作成されています。
いずれもCC0またはそれに相当するライセンスで提供されています。

## ライセンス
CC0

お望みの場合は[MIT](https://github.com/kurema/aozora2htmlSharp/blob/master/LICENSE)でのご利用も可能です。

## 関連
| レポジトリ | 概要 |
| -- | -- |
| [kurema/aozora2htmlSharp](https://github.com/kurema/aozora2htmlSharp) | 派生元。青空文庫形式をHTMLに変換。aozora2htmlのC#移植版。 |
| [aozorahack/aozora2html](https://github.com/aozorahack/aozora2html) | 上記の派生元。 |
| [kurema/AozoraGaijiChukiXml](https://github.com/kurema/AozoraGaijiChukiXml) | 他の青空文庫関連。[青空文庫・外字注記辞書【第八版】](https://www.aozora.gr.jp/gaiji_chuki/)のXML形式。 |