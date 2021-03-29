# Popcap Patches

A tool for fixing 3D acceleration of Popcap games in Windows 10, based on [cheez3d/popcap-patches](https://github.com/cheez3d/popcap-patches/).

This tool only help you make patched game file. For the full process, please refer to the [original repo](https://github.com/cheez3d/popcap-patches/tree/master/popcap-games/bejeweled-3).

## Games Support
`*` stands for not perfectly supported, see [Known issues](#known-issues) below for more info.

- Bejeweled 2 Deluxe
- Bejeweled 3
- Bejeweled Blitz
- Bejeweled Twist
- Zuma Deluxe *
- Zuma's Revenge *
- Peggle Deluxe *
- Peggle Nights *

## Known issues
- For Peggle series and Zuma Deluxe, run patch only if you don't need full screen. These games cannot be both full screen and 3D accelerated.
- In Zuma's Revenge, if your screen aspect ratio is different than the game, game area may get  clipped when running in full screen mode. (expected "contain" filling mode but actually get "cover")
- May not work with Origin version, or languages other than English.

### Need configuring
#### Peggle Deluxe
Add 2 more `*` in _Advanced Options > Video Card Check Pattern_.

`8B * 50 EB 03 8D * * * * * * FF FF 84 C0` → `8B * 50 EB 03 8D * * * * * * * * FF FF 84 C0`

See [issue #1](https://github.com/the1812/Popcap-Patches/issues/1) for details.

## Download
Please visit [Releases](https://github.com/the1812/Popcap-Patches/releases) page.

## Usage

### Make patch file
- Open `PopcapPatches.exe`
- Backup original game `exe` file
- Click `Open file` to select the game file
- Click `Patch` to get a patched file
- Replace original game file with patched file

### Clean up registry
- Open Windows Registry Editor, navigate to `HKEY_CURRENT_USER\Software\PopCap\<Game Name>`
- Remove `Test3D` key

### Modify config
- Backup `compat.cfg` in game folder (if there is, or you can skip this step)
- Open it with any text editor
- Find all functions about 3D and resolution, like `Is3DSupported`, `Is3DRecommended`, `IsHighResSupported` or `IsUltraResSupported`
- Let them just `return true`

Example:
```
function(bool) Is3DSupported
{
	if ((compat_D3DVendorID == VENDOR_SIS) || (compat_D3DVendorID == VENDOR_MATROX))
	{
		log("Returning Is3DSupported false due to SIS or Matrox hardware");
		return false;
	}

	// Nowadays any other hardware within our minspec range should be okay for 3D
	return true;
}
```
will become:
```
function(bool) Is3DSupported
{
	return true;
}
```
- Finally save the config file and run game

## Other info
Built with Visual Studio 2019, .NET Framework 4.7.2
