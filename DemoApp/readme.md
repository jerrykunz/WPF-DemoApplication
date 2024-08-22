Features:

1) Custom window (WindowStyle=None) with rounded corners, custom icons and all functionality written from scratch.
   Functions include:
	- Resizing
	- Dragging
	- Minimize/Maximize
	- Double click center of upper window to maximize
	- Alt+Enter fullscreen mode
	- Alt+F4 close
	- Close

2) Navigation bar that has these functionalities:
	- Hideable navigation bar
	- Dynamic language change 
		- supports infinite languages, each with own flag picture
	- Zoom button 
		- per theme amount of zoom levels, 3 by default, supports infinite amount
		- changes element sizes, customizable per element via style .xamls
	

3) Logging with a customized log4net 
	- Logs to files and syslog
	- Configurable from settings

4) Popups using modals
	- Can be tested via 'Popups' in navigation

5) Charts using LiveCharts
	- Generates random data, shows on live energy chart, saves to SQLite database for each minute
	- Retrieves data from SQLite database on entering view, shows this data in 1/10 minute average charts if within 10 minutes of current moment
	

6) CRUD SQlite
	- Encrypted with a password
	- Create, read, update and delete records as you wish

7) Multipage form that keeps data

8) Navigation Engine
	- You can navigate based on the wanted view or viewmodel
	- If navigating via views, many views can share the same viewmodel, making the codebase simpler
	- You can define whether previous viewmodels are disposed or kept in memory waiting for the next time it is navigated into, keeping the previous state or resetting on entry
	- Views can be based on loose .xaml-files or you can have the static views embedded within the program
	- The program remembers a customizable amount (default 8) of previous views, making it possible to backtrack using 'Previous'-button

9) Styles
	- You can define the folder where to retrieve the style .xamls from, each folder should have all the styles used by the application
	- You can define an infinite amount of new styles that customize the elements appearance, and you can change them in runtime per folder
	- Each theme has as many zoom levels as you wish to define
	- Use in combination with loose views to completely change the appeareance of the program
	
9) Loose views
	- You can define the folder where to retrieve the loose views .xamls from, each folder should have all the views used by the application
	- You can define an infinite amount of new loose view folders that customize the elements position and appearance, as well as new elements entirely, and you can change the used folder in runtime
	- You could also choose per view whether to use a loose .xaml, or the application embedded version
	- Use in combination with styles in order to completely change the appeareance of the program
	
10) Settings
	- Can be used like a default .NET application (settings per user in C:\Users\<YourUser>\Appdata\Local\<ProgramName>)
	- Default .NET style can also be set to use roaming network profiles
	- Settings can also be portable, located in the program folder (...\DemoApp\DemoApp.config)
	- Portable settings can be PC-agnostic (DemoApp.config holds the same settings for all PCs using this folder) or per PC (DemoApp.config file holds settings for all different PCs using this folder)
	- You can define these for a whole settings class or each setting individually, portable or not

11) DelegateCommand & RelayCommand
	- DelegateCommand availability must be manually updated via RaiseCanExecuteChanged(), through which it checks the canExecute function
	- RelayCommand does the checking automatically according to UI events, you have less control when using this one
	- If you have e.g. a string argument for your ICommand function you can use 'DelegateCommand<string>(FunctionWithStringArgument)', works with RelayCommand as well

12) Activity checking and timeout

settings josta laitetaan:
	- languaget
	- teemat