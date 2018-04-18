Sahara.Core.Logging.AccountLogs
=====================


####TransactionLog (Optional)

All logs are duplicated to the **TransactionLog**. This merges all logging that is usually split among different log types (Customer, Product, Etc.) into 1 master log of transactions. This gives you the ability to query for data such as *"all transactions by a user"*, or *"all transactions by time"*, regardless of **AccountLogType**.

####Adding a new AccountLogType.

* Configure the `AccountLogName` enum within **PublicTypes > AccountLogTypes** with the new log name: `[NewLogName]Log`.

* Configure the `ItemType` enum within **PublicTypes > AccountLogTypes** with the new item type: `[NewLogName]`.

* Create a new class derived from `AccountLogType` named: `[NewLogName]Log`

* Within `[NewLogName]Log`: Assign `LogName` equal to the new `AccountLogName` for `[NewLogName]Log` enum created above.

* Add the activites you wish to track by creating a static class called: `[NewLogName]Log` and creating an instance of `[NewLogName]Log` for each activity you wish to track and passing the name of the activity into the constructor using lowercase letters and dashes only. It`s best to be as descriptive as possible. It is also best to differentiate names so it is easier to distinguish between activities when merged in a master **TransactionLog** (if used).

You can now write to and read from logs using the new **LogType**. The enums you created will also ensure that your new logs are included when clearing all logs for an **AccountID**.



####Adding a new PartitionKey logging/filtering type

* Create a new class derived from `AccountLogTableEntity_Base` named: `AccountLogTableEntity_By[NewPartition]` within **AccountLogs.TableEntities**

* Within `AccountLogTableEntity_By[NewPartition]`: Assign RowKeys/PartitionKeys appropriately. A new TableEntity column may need to be added to accomidate your new partitioning needs. (If so: follow the instructions in the next section below)

* Configure the `AccountLogPartition` enum within **PublicTypes > AccountLogTypes** with the new **PartitionKey**.

* Update the PrivateMethod `WriteAccountLog` by creating an instance of `AccountLogTableEntity_By[NewPartition]`

* Add this new instance to the List<Object> `entityTypes` so that it is included in the insert table operation when looped through.


You can now use the new `AccountLogPartition` enum to get logs by the new **PartitionKey** using the `AccountLog.GetLogByPartition()` method.


#### Adding a new TableEntity column

* Add a new abstract property to the `AccountLogTableEntity_Base` within **AccountLogs.TableEntities**

* Add this property to all classes derived from `AccountLogTableEntity_Base`


