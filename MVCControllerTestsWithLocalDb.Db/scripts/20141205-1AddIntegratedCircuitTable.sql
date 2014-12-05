create table IntegratedCircuit (
	Id int not null identity (1,1),
	Code nvarchar(max) not null,
	Description nvarchar(max) not null
)
alter table IntegratedCircuit
	add constraint PK_IntegratedCircuit primary key clustered ( Id ) 
