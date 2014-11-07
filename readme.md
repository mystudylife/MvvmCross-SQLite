### Fork Info
This is a fork of the MvvmCross plugin, combined with the extensions from https://bitbucket.org/twincoders/sqlite-net-extensions

There are a number of fixes and performance improvements including:
- ManyToOne foreign keys for collections
- SoftDeletionAttribute (only implemented in ReadOperations.cs when loading related entities)
- Reducing number of queries to database
- Addition of async connection locking
- Added 2 [`ExecuteDeferredQuery`](https://github.com/mystudylife/MvvmCross-SQLite/blob/experimental/Sqlite/Cirrious.MvvmCross.Community.Plugins.Sqlite/SharedFile/SQLiteNet.cs#L2240) overloads that allow:
  - Table joins to be mapped in a single query
  - Rows to be mapped to a non-table entity
- Added [`GetWithChildrenInSingleQuery`](https://github.com/mystudylife/MvvmCross-SQLite/blob/experimental/Sqlite/Cirrious.MvvmCross.Community.Plugins.Sqlite/Extensions/ReadOperations.cs#L23) (experimental) extension method that uses the above mentioned method to automatically load related entities in a single query (ManyToOne and OneToOne only).

The long term goal is to refactor this into manageable chunks whilst expanding sqlite-net's support for foreign keys. It's likely for a while it will stay in a state where it serves our projects needs until we get some time to give it some TLC.

#### Sqlite-Net Extensions License
Copyright (C) 2013 TwinCoders S.L.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
