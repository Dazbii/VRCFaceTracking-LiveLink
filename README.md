# 👀 VRCFaceTracking LiveLink Module

This is a module for the VRCFaceTracking mod that enables you to use the FaceID sensors found on the IPhone X and newer to have face tracking with compatible avatars in VRChat desktop mode.

## 💾 Installation

Drag the `VRCFT_Module_LiveLink.dll` into your `%AppData%\Roaming\VRCFaceTracking\CustomLibs` folder

## 🔧 Configuration

~~The default port is set to 11111, which is the port LiveLink will use by default. To change this, run the mod once to generate preferences, then change the `LiveLinkPort` preference under the `VRCFT LiveLink` category in the config file found at `UserData/MelonPreferences.cfg`.~~

There is no way to change the port in the current version

## ▶ Usage

You need an IPhone X/XS/XR or newer, 12.9-inch IPad Pro 3rd gen or newer, or 11-inch IPad Pro 1st gen or newer to make use of this module.

- Install MelonLoader and the VRCFaceTracking mod
- Install this LiveLink module
- Install the app "[Live Link Face](https://apps.apple.com/us/app/live-link-face/id1495370836)" by Unreal Engine on your apple device
- Ensure that your apple device is connected to the same network as your computer
- Open Live Link Face on the apple device, and open settings, then tap Live Link at the top
- Add your computer's local IP address here. ~~and set the port if needed~~ (There is no way to change the port in the current version, so leave it as the default `11111`)
- Return to the main screen and make sure the Live button at the top is green
- Start VRChat in desktop mode, equip a VRCFT-enabled avatar, and enjoy facial tracking!

## 🔍 Troubleshooting

- Right click, open the properties of the .dll, and check this box if you see it to unblock the file
![](/images/unblock_dll.png "")
- Double check to make sure that your apple device and computer are connected to the same network
- Double check the IP address and port number entered in LiveLink match your computer's local IP address and that the port is either left blank or set to `11111`
- Check that the IP address enetered is the local IP for the shared network, and not for any other networks your computer may be connected to (e.g. Hamachi, Public IP)
- Ensure that your avatar supports VRCFT, and check in the toggles to make sure it is enabled
- Check your windows network settings, and ensure that the network is set as a private network

## 🧰 Other Useful Tools:

If you would like to bring your head movements into VR, and maybe hand movements as well with virtual controllers, check out [Driver4VR](https://store.steampowered.com/app/1366950/Driver4VR/). I have also worked on a module for [opentrack](https://github.com/opentrack/opentrack) that allows you to utilize the head rotation data from LiveLink to move the virtual headset, though it is not yet published.

## 👋 Credits

* [Unreal Engine Live Link Face](https://apps.apple.com/us/app/live-link-face/id1495370836)
* [benaclejames/VRCFaceTracking](https://github.com/benaclejames/VRCFaceTracking)
* [HerpDerpinstine/MelonLoader](https://github.com/HerpDerpinstine/MelonLoader)
