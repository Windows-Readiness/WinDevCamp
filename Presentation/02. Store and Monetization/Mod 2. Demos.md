#Module 2. Store and Monetization
##Demo 1. Video Ads

1. Open Visual Studio 2015
1. From the new project wizzard, choose Create a blank application (Using the Windows 10 blank template)
1. From Data toolbox menu, add a button to the design surface
1. Add a reference the the Micrsoft Advertising SDK for XAML
1. In the button click event handler, get the IntertitialAd object
1. Add an ErrorOccured event handler to check if an error occured
1. Add a Cancel event handler to check if the user canceled the video
1. Add a Completed event handler to check that user has viewed the video
1. Add a AdReady event handler and call ad.show()
1. Add a unitID and appID variable to store the video information
1. Call the RequestAd method using appID and unitID and the Video Type
1. Set breakpoints in the event handlers
1. Start the program
1. Click the button
1. The ad starts playing
1. Wait for the video to finish
1. When the video is finished, notice that the Completed event handler breakpoint is hit

##Demo 2. In App Purchase

1. From the new project wizard, choose Create a blank application (Using the Windows 10 sample template)
1. Start the application
1. Note that the sample template contains a series of todo lists
1. From MainPage.xaml, insert in the Hub control a HubSection that contains a DataTemplate with a Rectangle
1. Start the application
1. Notice the rectangle appears. That is the place where we want to place the ads
1. Bind the Remove ads button to an event handler named RemoveAdsComand
1. From the Main Page View Model, use the Show Ads snippets
1. From the Main Page View Model, use the Command snippets
1. From the Toolbox, bind the Visibility property of the HubSection to the ShowAds using a converter
1. Set the default value of ShowAds to true
1. Start the application
1. Click on shop, notice that the add has been removed
1. From the solution window, Add a new InAppPurchase Service Folder
1. Create a class named InAppPurchase Service
1. Paste the appropriate code
1. Back inside the view Mode, create an instance of this service. Use the id of the app in the store
1. Start the application
1. Click the shop button
1. Notice that a Windows Store window appears (it using a Simulator, not the real store)
1. Click OK
1. Notice that the Ad has disappeared. 
