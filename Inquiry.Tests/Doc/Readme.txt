SS 29 Oct 2011: These tests are no longer relevant after Inquiry went through major refactor. We should just delete this project.

What are we testing:

HomeController, DetailsController, ArchiveController:

- Pass a scan to the Home.Index method, and verify that it redirects to correct action/controller/id/pk1/pk2.
- Pass an invalid scan type to home controller. Should get redirected to Index View of Home Controller.
- Pass an ambigouous scan to home controller. Should  display Disambiguate View of Home Controller.
- Pass Scan to Details.Handle{x}Scan function. Make the query return valid input. Verify that model state is valid and every element matches.
- Pass Scan to Details.Handle{x}Scan function. Make the query return invalid input. Verify that model state is invalid and correct elements are invalid.

HomeRepository Tests

- Select a possible scan from the database
- Call GetScanType() and assert that the scan type is what you expect.
