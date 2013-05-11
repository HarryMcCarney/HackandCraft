## Hack and Craft Config ##

This creates a global singleton object which can be used for storing all config. The config is loaded in key value pairs from the DB.

You must run the BuildDbObjects.sql script into the db first. The package depends on dbvc and willl use the connection string in the projects config file.

Once installed the config can be accessed as follows 

    var mysetting = Globals.Instance.settings["mySetting"];

Enjoy

