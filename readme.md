### Fork Info
This is a fork of the MvvmCross plugin, combined with the extensions from https://bitbucket.org/twincoders/sqlite-net-extensions

There are a number of fixes, some improvements (ie. ManyToOne foreign keys for collections, reducing number of queries to database) and the additional of async connection locking.

The long term goal is to refactor this into manageable chunks whilst expanding sqlite-net's support for foreign keys. It's likely for a while it will stay in a state where it serves our projects needs until we get some time to give it some TLC.

#### Sqlite-Net Extensions License
Copyright (C) 2013 TwinCoders S.L.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.