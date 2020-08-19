# CSharp_PostgreSchemaChange

a) connect to PostreSQL Db

b) retrieve a list of tables

c) iterate the list of tables

    For each Table:

    > Iterate Columns:

      - change Column Names: add a "2020" to each name

      - change column order: move last two columns, to be first columns in table

    > Change TableName: add "2020" to tableName

    > LOG each schema change operation to text file: Dbname, TableName, Column Name: Action taken

d) purge PostgreSQL transaction logs (delete logs showing schema changes
