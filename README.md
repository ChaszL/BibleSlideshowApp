# Bible Slideshow App

A Windows desktop app for projecting Bible verses during a sermon. You run the controller window on your own screen and send verses, a sermon title, and a background image to a full-screen presenter window on a second monitor or projector.

## Features

- Load a whole Bible from a plain `.txt` file
- Jump to any verse by picking the book and typing the chapter and verse
- Double-click a verse to put it on screen
- Show the sermon title as a slide
- Set a custom background image (`.jpg`, `.jpeg`, `.png`, `.bmp`)
- See a live preview of what's on the presenter screen
- Move the presenter window between monitors

## Download

1. Go to the [Releases page](https://github.com/ChaszL/BibleSlideshowApp/releases/latest) and download `BibleSlideshow.exe`.
2. Get a Bible text file. Sample Bibles are already included in the source code, so you can [use those](#sample-bibles-included) or download one from [OpenBible.com](https://openbible.com).
3. Double-click `BibleSlideshow.exe` to start it. There's nothing to install.

The `.exe` is a single, self-contained file for 64-bit Windows, so you don't need to install .NET first.

> **Windows SmartScreen:** The `.exe` isn't code-signed, so Windows may show "Windows protected your PC" the first time you run it. Click **More info**, then **Run anyway**.

## Using the app

1. Click **Load Bible File (.txt)** and pick a Bible text file (see [Bible text files](#bible-text-files) below).
3. Click **Open Presenter Window**. Use **Change Monitor** if it opens on the wrong screen.
4. Choose a book, chapter, and verse, then double-click a verse to show it.
5. Press **Esc** on the presenter window, or click **Close Presenter**, when you're done.

## Building from source

You need Windows and the [.NET 10 SDK](https://dotnet.microsoft.com/download).

- **Run it:** Open `BibleSlideshow.slnx` in Visual Studio, or run:
  ```powershell
  dotnet run --project BibleSlideshow
  ```
- **Build the `.exe`:** In Visual Studio, right-click the project, choose **Publish**, and use the included `FolderProfile`. You can also run:
  ```powershell
  dotnet publish BibleSlideshow -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
  ```
  The `.exe` ends up in `BibleSlideshow/bin/Release/net10.0-windows/win-x64/publish/`.

## Bible text files

### Sample Bibles included

The source code includes two ready-to-use Bibles in the [`BibleSlideshow/bible_txts/`](BibleSlideshow/bible_txts/) folder:

| File | Translation |
| --- | --- |
| [`OpenBible.com_kjv.txt`](BibleSlideshow/bible_txts/OpenBible.com_kjv.txt) | King James Version (KJV) |
| [`OpenBible.com_akjv.txt`](BibleSlideshow/bible_txts/OpenBible.com_akjv.txt) | American King James Version (AKJV) |

The `.exe` download doesn't include them. You can get them either way:

- **One file:** Click the file name above, then click the **Download raw file** button (the down-arrow icon) at the top right of the file view.
- **Everything:** On the repo's main page, click the green **Code** button, choose **Download ZIP**, and look in `BibleSlideshow/bible_txts/`.

Save the `.txt` file anywhere, then load it in the app with **Load Bible File (.txt)**.

### Download Bibles from OpenBible.com

For the best results, download your Bible text files from **[OpenBible.com](https://openbible.com)**. The app is built around their plain-text format, so those files load cleanly. Text files from other sites may not load, or may load with missing books or verses.

### Required format

Every Bible `.txt` file **must follow the same format as the sample files in [`BibleSlideshow/bible_txts/`](BibleSlideshow/bible_txts/)**:

- `OpenBible.com_kjv.txt` (King James Version)
- `OpenBible.com_akjv.txt` (American King James Version)

Each verse is on its own line, laid out as **book name, a space, `chapter:verse`, a tab, and then the verse text**:

```
KJV
King James Bible: Pure Cambridge Edition - Text courtesy of www.BibleProtector.com
Genesis 1:1	In the beginning God created the heaven and the earth.
Genesis 1:2	And the earth was without form, and void; and darkness [was] upon the face of the deep. ...
Genesis 1:3	And God said, Let there be light: and there was light.
```

Rules to follow:

- **One verse per line.** A verse that runs onto a second line won't load correctly.
- **Use full book names**, like `Genesis`, `1 Samuel` or `Song of Solomon`. Numbered books use a digit followed by a space (`1 John`, not `I John` or `First John`).
- **Separate the chapter and verse with a colon** (`3:16`).
- **Put a tab between the reference and the verse text.** This is what OpenBible.com files already use.
- **Header lines at the top are fine.** Lines such as the translation name or a copyright note are skipped automatically, as long as they don't look like a verse reference.
- Blank lines are ignored.

If none of the lines match this format, the app shows *"Could not parse any verses. Ensure standard formatting."* If you see that, compare your file against the samples in `bible_txts/`.
