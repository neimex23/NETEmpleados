# NETEmpleados - Gestión de Empleados y Secciones

Este proyecto es una aplicación de escritorio desarrollada en **C# (.NET Framework 4.8)** con **Windows Forms** y **PostgreSQL** como base de datos. Permite realizar el mantenimiento (ABM) de empleados y secciones.

## 📋 Requerimientos

### Empleados
- **Código**: numérico, máximo 9999 (clave primaria)
- **Nombre completo**: hasta 100 caracteres
- **Fecha de nacimiento**
- **Fecha de ingreso**
- **Salario por hora**: numérico, con 4 enteros y 2 decimales
- **Baja lógica**: booleano (empleado oculto si está en baja)
- **Sección**: relación con la tabla de secciones

### Secciones
- **Código**: autoincremental (clave primaria)
- **Nombre**: hasta 100 caracteres
- **ID del empleado responsable**: máximo 9999 (nullable)

## 🧩 Funcionalidades

- Pantalla para ABM de **secciones**
- Pantalla para ABM de **empleados**
- Pantalla de **consulta general** con:
  - Tabla de todos los empleados activos
  - Cálculo de **antigüedad** en años (basado en la fecha de ingreso)
  - Cálculo de **adicional** al salario por año trabajado (1% por año desde el 4to año en adelante)

### 🧮 Ejemplo de cálculo adicional
Si un empleado cobra $100 por hora e ingresó en 2010, en 2024 tiene 10 años desde 2014, por lo tanto el adicional sería del 10%, es decir **$10**.

## ⚙️ Requisitos técnicos

- .NET Framework 4.8
- Windows Forms
- PostgreSQL
- Script SQL incluido (`NetDB.sql`) para crear la base de datos

## 🚀 Instalación

1. Restaurar la base de datos en PostgreSQL usando el script SQL proporcionado.
2. Abrir la solución `NetEmpleados.sln` en Visual Studio.
3. Compilar y ejecutar la aplicación.
4. Asegurate de tener configurada la cadena de conexión en `App.config`.

## 🧼 Recomendaciones

- Antes de comprimir el proyecto para entrega, **eliminar las carpetas `bin/` y `obj/`**.
- Cambiar el ícono del programa por el archivo `EmpresaDemoLogo.ico`.

---

Este proyecto fue desarrollado como parte de una **prueba técnica de selección** y puede ser utilizado como base para futuros sistemas de gestión.
