CREATE USER dev WITH PASSWORD 'root' SUPERUSER;

CREATE DATABASE netempleados;
\c netempleados; 

create table empleados(codigo int Primary key, nombre varchar(100) not null, fechaNac date not null, fechaIng date not null, salarioHora decimal(6,2), bajaLogica boolean default false, seccion serial not null);
create table secciones(codigo serial primary key, nombre varchar(100) not null, idResponsable int); 

alter table empleados
add foreign key(seccion) references secciones(codigo) ON DELETE CASCADE ;

alter table secciones
add foreign key (idResponsable) references empleados(codigo) ON DELETE SET NULL;
