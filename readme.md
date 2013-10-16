### Virblue Fork Info
This is a fork of the MvvmCross plugin, combined with the extensions from https://bitbucket.org/twincoders/sqlite-net-extensions

There are a number of fixes, some improvements (ie. ManyToOne foreign keys for collections) and the additional of async connection locking.

The long term goal is to refactor this into manageable chunks whilst expanding sqlite-net's support for foreign keys. It's likely for a while it will stay in a state where it serves our project's needs until we get some time to give it some TLC.


##MvvmCross Sqlite-Net

An attempt to allow the community to more easily contribute...

This repository is a fork of the wonderful https://github.com/praeclarum/sqlite-net/

This repository relies on 'core' nuget packages from https://github.com/slodge/mvvmcross

We're looking for contributors and Admins for this project.

Some of the TODOs on the list are:

- add Async - @Praeclarum and @Jarroda have already done the 'hard work' - we just need to work out how to include it - do we switch to Profile78? Do we drop WP7.5? Do we hack in AsyncBridge somehow?
- add Test harnesses - @Praeclarum and team have done this already - we just need to merge back here
- consider extensions - like https://bitbucket.org/twincoders/sqlite-net-extensions
