# Newest Update: No longer slowing down program, 1 second seems to be sufficient enough to not make too many calls to be banned by the binance system for 5 minutes.

## Fractional Trade Bot
To use the application, initially run the application and you should recieve an error stating "no user authentication file found".
The application will then create a default xml file called "UserAuthentication.xml" (DO NOT SHARE) onto your desktop.

The program is designed to run 24/7, although periodic syncing of the system clock is needed every so often (once a week).

# Fig 1
![What you should first recieve: Invalid File](https://sylascoker.co.uk/img/invalidFile.PNG)

# Fig 2
![Where to find the file](https://sylascoker.co.uk/img/fileLocation.PNG)

## How to use the "UserAuthentication.xml" file:
* open the file
* Decide whether you would like to use the "Test" Servers (recommended) and or the live marketplace servers.
* Fill in the appropriate attribute values with your secure API keys either from the Binance Test server or Marketplace.

# Fig 3
![What to put into the attribute values](https://sylascoker.co.uk/img/fileTutorial.PNG)

## What to expect once you've successfully passed the authorisation stage
- The program will automatically start the "BuySystem" static class,
- It will then find all coins that're tradeable in the marketplace,
- It then creates a thread to get information about the specific coin (i.e. rolling 24 window price change in percentages),
- Compares the coin to the performace of the average coin, i.e. is the price change positive? It is? Is the price change greater than the average price change? It is?
- Place a market order for the coin.
- Check your binance wallet for the now converted asset!

# Fig 4
![Enjoy](https://sylascoker.co.uk/img/finalStage.PNG)

## Todo List
- Lock the 'TotalPercentageChange' field from other threads to keep it updated and valid. although doesn't seem to be a problem as of now.
- Create a Test environment to check if the coins that are being bought eventually are the Top 5 performing in the marketplace (because I have a theory and it seems to be matching up, but I need to check either way).
- Create feature to auto deposit cash into wallet when there are insufficient funds.

## Mobile app Todo
* When I have time:
- Adding Frontend (Xamarin) to give a more "user friendly" UI.
