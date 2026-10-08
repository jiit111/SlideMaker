# Tesseract language data

`TesseractOcrService` (see `Services/OCR/TesseractOcrService.cs`) needs the trained-language
files for the languages configured in `OCR:Languages` (default `eng+hin`) to be present in
this folder. They are not distributed with the NuGet package, so they must be downloaded once
per machine:

1. Download `eng.traineddata` and `hin.traineddata` from
   https://github.com/tesseract-ocr/tessdata (or `tessdata_fast` / `tessdata_best` for a
   smaller/larger, faster/more-accurate variant).
2. Place both files directly in this folder (`App_Data/tessdata/`).

Until these files are present, image upload will show a friendly message and let you enter
questions manually instead of failing the request.

The folder/language list is configurable via `OCR:TessDataPath` / `OCR:Languages` in
`appsettings.json` — nothing is hardcoded.
