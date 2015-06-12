create table BgServiceFlag (
	Id int not null,
	LastStarted DateTime null
)
alter table BgServiceFlag
	add constraint PK_BgServiceFlag primary key clustered ( Id ) 
