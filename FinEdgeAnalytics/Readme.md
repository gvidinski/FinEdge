# FinEdgeAnalytics

## Application overview
The project uses .NET 9.0 framework and is a simple console application that simulates a data processing pipeline using layered architecture.
There are three Transformers, three Extractors and one Loader. The Loader is unaware of the Extractors and Transformers and
it will instantiate every Transformer that implements the "ITransformer" interface", wich makes posible to add more new Transformers
easily. On the other hand every Transformer instantiates single Extractor that implements the "IExtractor" interface, again making posible to add
more Extractors easily, but is customized to work with particular Transformer sharing own Dtos.
Inside the project at the moment there are three pipelines, which will extract data from a CSV file, external DB simulated using
EF's InMemoryDatabase and a placeholder of an example of external API pipeline, whuich returns an empty list for the sake of the example.
In order to be able to work with huge mount of data the hole process is developed around yielded result sets, which are passed from one
component to another. The Loader is started in its own Task in a non-blocking way, sharing the application's context.
For the Dtos and Models I've used record types, which are immutable and are a good choice for the purpose of the project.
Inside Transformers there is a placeholder for filtering the data through LINQ which has hardcoded conditions for the purpose of the test,
but it can be easily extended and changed to work with dynamic conditions from the configuration file or from the UI.

## How to run the application

### Prerequisites
The project uses MSSQLLocalDb instance but it can be easily changed to use different SQL server instance.
First we need to create the database using the following scripts from the "Data" folder incuded in the project
in the followin order:
- CreateDb.sql - creates the database along with the table "Transactions". Also adds five records to the table.
- SP.sql - creates the stored procedure "InsertOrUpdateTransactions" and also creates type "TransactionType" for the
Table-Value parameter used by the stored procedure to insert or update multiple records in the "Transactions" table.

In case we want to use different SQL server instance we'll need to change the default connection string inside the
configuration file 'appsettings.json'.
The next thing we need to have installed is .NET 9.0 framework.

### Steps to run the application
When the application is started we need to place a HTTP GET request to the folloing ULR from any browser
or platforms like [Postman](https://www.postman.com/downloads/):\
URL: [https://localhost:7220/run](https://localhost:7220/run)\
Right after that we should receive Response OK with status code 200 and message "Runner started.". If we send
another request to the same URL while Runner is still running we should receive Response OK with status code 200
and message "Runner is still working.".

## Future improvements
The following list shows some of the improvements that can be made to the application:
- Switch to a logger, like [Serilog](https://github.com/serilog/serilog/wiki/Getting-Started), to store the log
information into a file.
- Introduce plugin like system where every Loader, Transformer or Extractor will hav their own projects and their libraries will be loaded dynamicaly.
- Introduce separate configuration file for each Loader, Transformer and Extractors.
- Add functionality to run more than one Loader.
- Add functionality to monitor Runner's task state. If dynamic update is needed by the UI then SignalR can be used.
- Add localisation to the application strings through resource files.
- More tests, including Integration ones

