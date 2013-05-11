DBVC
============
A lightweight stored procedure based orm.
Uses Sql Server's xml support to enable parsing complex objects to and from the DB via stored procedures.
This gives a big performance advantage over traditional orms as data access can be optimised by keeping one api hit or page load to one db call and tuning the sql directly in the stored proc.
It also keeps the business logic modular and close to the data.

All stored procs take the form of 

    create proc dbo.my_proc
    @p xml = null,
    @r xml output
    as
    begin try
	//some logic here
    declare @user xml

    select @user = (
				select 'Ludwig' as "@name"
				for xml path ('User), type
				)

    select @r = (
					select 0 as "@status", 
					object_name(@@procid) as "@procName",
					@user
					for xml path ('Result')
					)


     end try
    begin catch
		exec dbo.set_error @p, @r output
    end catch

In the c# layer we create a result class which implements dbvc.IResult and wraps our mapped object classes.

	[Serializable]
    public class Result : IResult
    {

        [XmlAttribute]
        public string errorMessage { get; set; }
        [XmlAttribute]
        public string procName { get; set; }
        [XmlAttribute]
        public int status { get; set; }
        [XmlAttribute]
        public string dbMessage { get; set; }
        [XmlElement]
        public User User { get; set; }
    }

	[Serializable]
    public class User
    {
        [XmlAttribute]
        public string name { get; set; }
	}


Now we can call the proc as follows
 
      var orm = new Orm();
      var result = orm.execObject<Result>(null, "dbo.my_proc");

We can pass a param object to the proc by replacing the null
	
    var user = new User();
	user.name = "Ludwig";
    var result = orm.execObject<Result>(user, "dbo.my_proc");

By default the orm uses the connection string named "ApplicationServices" in app settings.
This can be overridden in the code by adding 

    var connstr = @"data source=192.176.0.1\MSSQLinstance;Initial Catalog=[MyDb];User Id=[User];Password=[pwd];";
    var orm = new Orm(new MSSQLData(connstr));

This also means the MSSQLData class can be replaced by new adaptors which allow basic xml files or another db platform to be used as the source.

It is also possible to use an overload on the MSSQLData class to directly to save json data directly the db.

    var jsonProc = new MSSQLData();
    var jsonresult = jsonProc.execStoredProc("api.json_proc", JsonObjectId, jsonParam);

The JsonObjectId is the primary key of the table the json will be saved to. jsonParam is the json string to be saved.

jsonresult will contain a the returned json string.












