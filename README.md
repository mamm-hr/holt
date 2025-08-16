# Holt

Holt holds and vaults Git repositories locally for safekeeping.

This repository contains the source for the **Holt** backup service.
The service uses the .NET Generic Host and can run as a console
application, a Windows service, or as a systemd service on Linux.

Jobs are described by XML files placed in a configurable job directory
(defaults to `jobs`). Each job specifies the repository URL, branch,
local path, and the interval in minutes between synchronisations.
Holt monitors the job directory for changes and starts, restarts, or
stops jobs as the XML files are added, modified, or deleted.

Logging uses the EventLog provider on Windows and the EventSource
provider on other platforms.
