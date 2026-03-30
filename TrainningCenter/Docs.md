# There Is The main Instractions for the Page Handeling System
	## In the begining 
		1- In the Constractor We need to initialize all the properties that we will use in the entire process including the data context to 'this'.
		2- In the same Direction I have to make a helper functions to load the data for example People Data:
			2.1- we create a container for the original data.
			2.2- we lead the data from BLL to that container.
			2.3- After that in a foreach loop we convert each element to a VM "ViewModel" and add it to another container for the VM.
			2.4- If We Have any Statistics to be calculated we can do it in the same level of the method.
			2.5- Finally we apply the filtering in there if there is any filtering needed.
			2.6- In the Filtering process we have to make sure that we do not hitting the database every time we filter, 
			so we need two collections in the code behind one for the original data and one for the filtered data.
		3- If We have any Element that we need to load a data from the BLL we can create a helper function for that as well, and we call it in the Constractor.
		
	## At the level of the View Model
		1- We must include the `INotifyPropertyChanged` Interface to be able to notify the View about any changes in the properties.
		2- Every Property in the UI that we need to update it from the code behind and is related to the BLL class
		we have to create a property in the VM with the same name and type.
		3- In the Setter of the property we have to call the `OnPropertyChanged("PropertyName")` method to notify the View about the change.
		4- If we have any conversion between the BLL and the VM we have to do it in the Constractor of the VM.


# Quick Feature Comparison:

| Module     | Complexity | Impact   | Dependencies    | Time     |
|------------|------------|----------|-----------------|----------|
| Trainers   | Medium     | High     | None            | ~4 hours |
| Centers    | Low        | Medium   | None            | ~2 hours |
| Sessions   | High       | Critical | Trainers+Groups | ~6 hours |
| Attendance | Medium     | High     | Sessions        | ~4 hours |
| Payments   | Medium     | High     | Students        | ~4 hours |
|------------|------------|----------|-----------------|----------|