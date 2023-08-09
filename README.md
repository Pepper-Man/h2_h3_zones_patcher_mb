# h2-to-h3-zones-patcher
A program for patching H2 zones/areas/firing positions into a H3 scenario

This is based on my previous python script version, but now using ManagedBlam thanks to the support added in the latest update.
This drops the time required from at least 5 hours to a mere 30 seconds or so!

# Requirements
* Requires [.NET 4.8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)

# Usage
* Download the latest release, or compile your own version.
* Extract H2 scenario to XML with `tool export-tag-to-xml`.
* Make sure the destination H3 scenario has no zones blocks.
* **Copy your Halo 3 ManagedBlam.dll (found in "H3EK\bin") into the same folder as this exe**
    * Alternatively, simply place the files of this program directly into your "H3EK\bin" directory.
* Run this .exe, provide the file paths when prompted.