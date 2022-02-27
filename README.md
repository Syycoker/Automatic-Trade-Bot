# Fractional Trade Bot
To use the application, initially run the application and you should recieve an error stating "no user authentication file found".
The application will then create a default xml file called "UserAuthentication.xml" (DO NOT SHARE) onto your desktop.

# How to use the "UserAuthentication.xml" file:
* open the file
* Decide whether you would like to use the "Test" Servers (recommended) and or the live marketplace servers.
* Fill in the appropriate attribute values with your secure API keys either from the Binance Test server or Marketplace.

# What to expect once you've successfully passed the authorisation stage
- The program will automatically start the "BuySystem" static class,
- It will then find all coins that're about to be traded in the binance marketplace,
- It then creates a thread to find information about the specific coin and calculates its metrics using some math I haphazardly put together, i.e. its price change (%) in the last 24 hours
- If the system deems the coin acceptable, it will initiate a buy order for that asset.

# Making this project into a mobile app
* Todo list when I have time:
- Adding Frontend (Xamarin) to give a more "user friendly" UI for the user.
- Adding a "Sell System" to bring in profit*.
