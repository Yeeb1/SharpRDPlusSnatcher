# SharpRDPlusSnatcher
This tool exploits a flaw in the [Remote Desktop Plus](https://www.donkz.nl) application, which temporarily drops `.rdp` files in `%localappdata%/Temp` and fails to clean up these files efficiently. This allows to decrypt and extract credentials from these temporary files before they are passed to `mstsc.exe` (Microsoft Terminal Services Client). This program can only decrypt `.rdp` files of the current user, as the files utilize the Data Protection API (DPAPI) for encryption, which is tied to the user's session.
## Acknowledgments

## Compilation

Before compiling, add the `System.Security.Cryptography.ProtectedData` package to the solution. This package is necessary for interacting with the Data Protection API (DPAPI) to decrypt the credentials found within the `.tmp` files.

You can add this package via the .NET CLI with the following command:

```sh
dotnet add package System.Security.Cryptography.ProtectedData
```

Alternatively, if you're using Visual Studio, you can manage NuGet packages for your project and add `System.Security.Cryptography.ProtectedData` from there.

## Usage

Upon execution, this tool first searches for any `.tmp` files that might still exist as artifacts in the `%localappdata%/Temp` directory due to Remote Desktop Plus's failure to clean up. It attempts to decrypt and display any credentials found within these existing files. After processing the existing files, the tool then monitors the directory for any newly created `.tmp` files, decrypting and displaying credentials in real-time.

```
╔═╗┬ ┬┌─┐┬─┐┌─┐╦═╗╔╦╗╔═╗┬  ┬ ┬┌─┐╔═╗┌┐┌┌─┐┌┬┐┌─┐┬ ┬┌─┐┬─┐
╚═╗├─┤├─┤├┬┘├─┘╠╦╝ ║║╠═╝│  │ │└─┐╚═╗│││├─┤ │ │  ├─┤├┤ ├┬┘
╚═╝┴ ┴┴ ┴┴└─┴  ╩╚══╩╝╩  ┴─┘└─┘└─┘╚═╝┘└┘┴ ┴ ┴ └─┘┴ ┴└─┘┴└─

[+] Checking for artifact .tmp files in application data.
[Credentials Found]: C:\Users\yeeb\AppData\Local\Temp\Remote Desktop Plus.006F4.tmp
┌───────────────────────────────────────┐
│ Username: yeeb                        │
│ Password: JediMaster1234!             │
└───────────────────────────────────────┘
[Credentials Found]: C:\Users\yeeb\AppData\Local\Temp\Remote Desktop Plus.04BF4.tmp
┌───────────────────────────────────────┐
│ Username: admin                       │
│ Password: SithLordGalacticEmpire!     │
└───────────────────────────────────────┘
[+] Monitoring for new Remote Desktop Plus Connections. Press Enter to quit.
[File Copied for Processing]: C:\Users\yeeb\AppData\Local\Temp\e037061e-3fdc-47af-a5ef-4881d99774bd.tmp
[Credentials Found]: C:\Users\yeeb\AppData\Local\Temp\e037061e-3fdc-47af-a5ef-4881d99774bd.tmp
┌───────────────────────────────────────┐
│ Username: yeeb                        │
│ Password: RebelAlliance1977!          │
└───────────────────────────────────────┘
```

## Acknowledgments

Special thanks to the [rdp-file-password-encryptor](https://github.com/RedAndBlueEraser/rdp-file-password-encryptor) repository for providing guidelines on how to decrypt the contents of these temporary `.rdp` files.

---

*The script is for informational and educational purposes only. The author and contributors of this script are not responsible for any misuse or damage caused by this tool.* <!-- meme -->
