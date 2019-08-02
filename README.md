## EmailSender

**EmailSender** is a locally hosted web browser application that can send groups of emails from a user's email service provider.  It is Windows based and assumes that Microsoft .NET Framework 4.7.1 is installed.  Download and extract the `EmailSender-master` zip file from the [EmailSender repository](https://github.com/deandevl/EmailSender) . Locate and run the `setup.exe` setup file.  

Locate and edit the `settings.xml` file in the root of the installed directory.  There are xml tags for your `EmailFromAddress`, `EmailFromName`, `EmailImageContentID`, `EmailServer`,  `EmailPort`,  `EmailUser`, `EmailPass` (your email password).  Somewhere on your computer system create an html formatted email message.  If you are embedding an image, make sure that the `EmailImageContentID` in `settings.xml`is identical to the Content_Id value in your message html.  For an example message html file, see the `message.html` file and associated image file under the `TestMessage` directory.

To start the local html server, run the `EmailSenderApp.exe` executable by clicking either the desktop or start menu shortcut under `deandevl`.  From either a Chrome or Firefox browser enter `localhost:8082` for the address url.  The **EmailSender**'s main page should then be rendered.

At the top of the main page click the button labelled `Database folder...` .  A Windows folder dialog will appear so select a local directory to set the location of  your email database(s). Next to  the button is an editable combo box.  Enter a name for the database in the combo box and click the Enter key.  Toward the bottom of the main page in the input section (lower left corner) enter a Group, Name, and URL Address to create a destination email.  Click the Add button to add the address to the database.  The table above the input area should respond to the list of added email destinations.  Enter and add as many groups with email names and addresses as needed.

To test the application, click the `Set Message File...` button and select the `message.html`file from the TestMessage directory.  Click the `Set Image File...` button and select the `strawberrybasket.jpg` file from the same directory.  Enter a subject in the `Email Subject` input box.  If you like, click the `Check Addresses` button to check destination addresses' formatting and domain.  Finally click the `Send Emails` button to send the message to all email addresses listed in the destinations table.



