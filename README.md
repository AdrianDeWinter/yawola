# yawola (Yet another WakeOnLan App)
This is a small utility to wake/boot up hosts on the network which support WoL via a magic packet.

It is still work in progress, any feedback/contributions are more than welcome!

There are alread quite a few WoL apps on the Microsoft Store, however i just kinda wanted to build my own \**shrug*\*

Existing Features:
 - send magic packets to any
   - IPv4 address
   - IPv6 address
   - local network host name
   - URL
 - Add hosts via a nice little content dialog
 - Hosts are automatically saved to localstorage
 - Support x86, x64, ARM and ARM64

Planned features:
 - add option to use roaming storage. Configurable via settings page
 - make the number of attempts configurable
 - detect if the host was woken successfully? sadly, ping is not available to uwp applications, so this one is not quite straight forward
 - add an api to be called by other apps?
 - integrate with the windows terminal app to run things like ssh once the host is woken? (is that possible?)
 - fully responsive layout
 - come up with a design that is not purely white?
 - release to the store :P
 - add this app to the winget repository
