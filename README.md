# Cosmares
## Easily create Cosmos installers!

### Features:
- Throughly tested.
- Works with bin files.
- Can modify accent color of setup.
- Can properly read, format and install on drives. (Yes, even ones that are not in FAT32 and supported by Cosmos format)
- Works perfectly when installed on VM's.

### What's missing?
As of this edit of the README file, Cosmares is not supported with real HDD's and SSD's due to the fact that it does not write to the first boot sector (the one that makes the BIOS see the drive as bootable) and that is because as of right now I have no clue on how to write to the boot sector so... well its hanging like that for now.

### How to use:
Well, it's not that hard, you just have to `git clone` this repo or download it and then just open the `Kernel.cs` file and modify the configs above! And that's done! That's all you need to do! Then you can just build it (assuming that you have Cosmos installed) and then it will just work magically with its in-house installer pages.

### Why would you use it?:
To sum it up, three main points:
- Reduces all efforts in figuring out to make your own installer.
- Reduces stress of manually updating your OS installer.
- Easily configurable to your likings!

### What does it offer?:
It offers you a well-tested installer process that works right now perfectly with VMWare, VirtualBox and QEMU. It can properly format, write and install an OS for you and it's very easy to maintain.<br>
It also offers the UI customizations (not a lot as of right now) but you can modify accent colors of the setup right now.<br>
