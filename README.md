EmailAPI c# .NET Web API
Uses SendGrid to send Messages
uses QuestPDF to attach pdf On emails
Accepts a list of emails as strings
Accepts Report data as list
Sample Data:
{
  "emails": [
    "string"
  ],
  "reportData": [
    {
      "transactionReference": string,
      "truckNumber": string,
      "date": Datetime,
      "amount": Integer
    }
  ]
}
Runs on static port 5171
A standalone APi that can be called EXTERNALLY and provided with the content data 
