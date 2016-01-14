# Shameful Old Git

This is a project to practice things learnt during the first unit of the Akka.Net bootcamp.

Currently at work we use git flow and there are lots of branches that are old and not merged into develop. This project will be run as a service when my laptop starts up and hopefully shame people into cleaning the remotes up.

The code creates the required base level actors and decides whether enough time has elapsed since it was last run. If it is too recent then it will not run. If it has not run for a set amount of days then it will spin up new actors for each repository specified and then find the branches whose tips are not in the specified branch and spin up actors to get the info for these branches. All of this info is passed to an actor who gathers it all up and then passes all of the information to the printing and then emailing actors. Once it has been emailed it will record so into a text file.

This is a quick project to practice Akka-ing, it currently doesn't have any tests and may not get any.