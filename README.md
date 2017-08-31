### Build status
# [Download](https://github.com/shavitush/quest_optimizer/releases/latest)

# Quest Optimizer
This program automatically patch your Quest.wz and remove ALL quests except for the ones in `exclude.txt` (if any).  
Upon patching, the program will generate `Quest.wz.bak` and `Quest.wz.patched` and proceed to delete the input file.  
To run, you need a `Quest.wz` file in the same folder Quest Optimizer is in.

To use, put `Quest.wz.patched` into your MapleStory/appdata folder, delete `Quest.wz` from your MapleStory folder and rename `Quest.wz.patched` to `Quest.wz`.  
Before applying game upgrades (patches) or to revert this patch, do the same as above, but with `Quest.wz.bak`.

# Search Mode
In order to search some quest's id, please use this program in search mode:
Just double-click `search_mode.bat`.

# Build
- Needs [MapleLib](https://github.com/haha01haha01/MapleLib). I do it with Visual Studio 2017, cannot guarantee success with any other .NET compilers.