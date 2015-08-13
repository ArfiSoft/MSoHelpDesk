# MSoHelpDesk
Web interface for the Kaseya ServiceDesk

Just testing what is posible with the Kaseya en ServiceDesk API's.

Helpdesk is for watching helpdesk tickets for customers.
The can see all tickets issued by the customer.
OWIN.Identity is use for authentication.

ToDo:
- Fix ticket modal layout, maybe even transfom it into a view.
- Change grid to bootstrap table.

Intake is an intranet site using windows authentication.
With this site it's posible to create or update a ticket in the servicedesk from Kaseya.

ToDo:
- The popup for the autocompletion needs to be restyled.
- Show form inputs dynamicly. Done!!
- Get all ticket fields correct before the ticket is saved to Kaseya. Done!!
- Fix the scope issue, only the organizations and tickets are returnd from the scope of the user used to log in the Kaseya API.

