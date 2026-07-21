# Time Clock

Time Clock is a punch system management that allows organization to easily record the clock in/out timestamp of their employees across different departments and units with an `Unique ID`.


## Table of contents

- [Tech Stack](#tech-stack)
- [Features](#features)
- [Future Improvements](#future-features)
- [Development Steps](#development)
- [Production Steps](#production)
- [Documentation](#documentation)
- [Video Tutorial](#tutorials)

### Tech Stack

- **ASP.NET (v10)**: For server side code and admin panel
- **MS SQL(v9)**: Database through `SQL management Studio`
- **Blazor (v10)**: For punch system frontend connected to sever with `API`
- **Visual Studio**: As `IDE`
- **Remix Icons**: For icons
- **DotNetEnv (v3)**: For environment variable management
- **ClosedXML (v0.105.0)**: For excel report generation
- **Identity EFC (v9)**: For authentication and authorization management
- **Google Gemini / Claude AI**: For frontend styling and debugging

### Features

The project has following features:

- [x] Barcode scan to clock in/out flow
- [ ] Full fledge admin panel made in ASP.NET MVC
- [x] Role based policies for authentication and authorization using `Identity Core API`
- [ ] PTO management and request/approval system
- [x] Extensible `pagination` feature
- [ ] Extensible `filtering` feature
- [x] Pay Period generation with Day Time Shift adjustment
- [x] Multiple `job profiling` for easy transfer from unit to unit
- [x] Report generation by `unit` and by `profile`
- [ ] Kiosk management with IP restriction for enhanced security

#### Future features

Following are the future features, I will be working on to add more value to the system.

- [ ] A two-week time scheduler with drag and drop calender view
- [x] Easy active clock in view for employees

### Development

The project is based on the ASP.NET Core MVC pattern and heavily rely on the `visual studio` (not VS code) IDE for the development.

To get started follow the steps below:

> Clone the project
```bash
git clone https://github.com/alsongarbuja/TimeManager.git
```

> Open in Visual Studio

Open the `slnx` file in Visual Studio **(again not VS code for the best DX)**

> Create an env file on the root of the Backend project

Create a `.env` file on the **root of the Backend project folder (TimeManager.Backend)**. You can copy the `.env.example` file for reference and update the values with your credentials.

> Migrate the database

Run the following command (as per your environment) to migrate the database for initial setup.

`Inside Project Manager Console in Visual Studio`
```bash 
Update-Database
```

`Any terminal with dotnet installed`
```cmd
dotnet ef database update
```

> [!Tip]
> 
> If there are some error from database (SQL) saying no database then you have to create the database first.

> Configure the Startup cycle of the project

You can start the **TimeManager.Backend** and **PunchSystem** project separately now but for single click startup, you can configure the project to start both project together.

1. Right click the Root of the project in Visual Studio
1. Click Configure Startup Projects...
1. A popup modal will appear and under Configure Startup Projects (Common Properties) select the Multiple startup projects option
1. And update the *action* for **TimeManager.Backend** & **PunchSystem** to *`Start`* and others to *`None`*

Now you should be able to click the **start** button on the toolbar and start both projects together.

> Visit the URl

**Punch System**: [https://localhost:7187/](https://localhost:7187/)

**TimeManager.Backend**: [https://localhost:7263/](https://localhost:7263/)

### Production

To deploy the project, we will first run the `publish` command from `dotnet` then use the executable generated to deploy the project on IIS server.

```bash
dotnet publish -c Release
```

`If you want to run the project on non-window server. This command will generate the self-contained executables, but will be heavier since it ports .NET runtime too.`
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

Now follow the following steps [ASP.NET Publish To IIS](https://learn.microsoft.com/en-us/aspnet/core/tutorials/publish-to-iis?view=aspnetcore-10.0&tabs=visual-studio) to deploy the project on an IIS server.

### Documentation

For detailed documentation checkout this [documentation site](https://alsongarbuja.github.io/TimeManager-Doc)

### Tutorials

For tutorials on the development steps check out the [playlist]() on the youtube.

For tutorials on how to use the system after setup check out this [playlist]() on youtube.
