Sahara.Core.Logging.PlatformLogs
=====================



####Adding a new PlatformLogType.

* Configure the `PlatformLogName` enum within **PublicTypes > PlatformLogTypes** with the new log name: `[NewLogName]Log`.

* Create a new class derived from `PlatformLogType` named: `[NewLogName]Log`

* Within `[NewLogName]Log`: Assign `LogName` equal to the new `PlatformLogName` for `[NewLogName]Log` enum created above.

* Add the activites you wish to track by creating a static class called: `[NewLogName]LogActivity` and creating an instance of `[NewLogName]Log` for each activity you wish to track and passing the name of the activity into the constructor using lowercase letters and dashes only. It's best to be as descriptive as possible.

You can now write to and read from logs using the new **LogType**.

