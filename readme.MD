# SummarizeRobocopy

## Purpose:

Used to scan Robocopy output and show the highlights of the run,a summary.

## Justification

I got tired of trying to fingure out all of the nit-pick details of the various file backup
routines so I wrote my own script using Robocopy. I redirected Robocopy output to TXT file
and used task scheduler for nightly runs. I then discovered that checking the results was a pain.

And I had not written WPF code in a while, so this was a fun project.

So I have the Robocopy commands dump all the results to file and use this tool to provide a summary. If I don't like the summary, I can view the detailed file results.

## Command line systax

Examples:

SummaryRobocopy \junk\*.txt [\anotherfolder\ExactFile.txt] [\yetanoter\backup*.txt] [...]

It needs at least 1 file, but you can give it wild card or files as many as you want.

## How I use this:

- I have a CMD file running robocopy redirecting output to a TXT file.
  - Example: robocopy %From% %Dest% /E /R:2 /W:10 /NP /NFL /LOG:%logname%.txt /TEE /XD "\_\*"
- It's schduled to run at night
- Next day I run this program to open all of the nightly backups that I scheduled
  - being weird, I backup to my travel machine, and backup up to my backup machine, I have select folders I am interested in backing up.
- Here is my daily run commandline:
  - SummarizeRobocopy.exe c:\somefolder\\backup*txt c:\anotherfoldershared\backup*.txt
-

## Future plans

- Add Semantic Versioning , this is version 0.1.0
- color the border around the window
- provide a dark-mode Light-mode option, just for fun
- provide "Save" and "Copy To cClipboard" options
- It's currently uses .NET 8 runtime, see about backing down to .NET 4.8
- In the other direection, rewrite the UI using WinUI 3
- Try .NET MAUI and see what haopens in Linux
- VS 2022 created an EXE, a DLL, two JSON files which are neeeded to run this. Find a way to put everything just one file.
