import {getTodoManagerServiceInstance} from "../services/TodoManagerService.js";

const {WebcController} = WebCardinal.controllers;

export default class TodoListController extends WebcController {
    constructor(...props) {
        super(...props);
        getTodoManagerServiceInstance(this, (todoService) => {
            this.TodoManagerService = todoService;
            // Populate existing todos to item list
            this.populateItemList((err, data) => {
                if (err) {
                    return this._handleError(err);
                } else {
                    this.setItemsClean(data);
                }
                // Init the listeners to handle events
                setTimeout(this.initListeners, 100);
            });
        });

        // Set some default values for the view model
        this.model = {
            items: [],
            item: {
                id: 'item',
                name: 'item',
                value: '',
                placeholder: 'Type your item here'
            },
            filterString: {
                id: 'filterString',
                name: 'filterString',
                value: '',
                placeholder: 'Type here to filter'
            },
            'no-data': 'There are no TODOs'
        };

        this.sortAlphabetically = 0;
        this.sortCheckbox = 0;
    }

    initListeners = () => {
        const todoCreatorElement = this.getElementByTag('create-todo');
        if (todoCreatorElement) {
            todoCreatorElement.addEventListener("focusout", this._mainInputBlurHandler);
        }

        const itemsElement = this.getElementByTag('items');
        if (itemsElement) {
            itemsElement.addEventListener("focusout", this._blurHandler)
            itemsElement.addEventListener("click", this._ToDoClickHandler)
            itemsElement.addEventListener("dblclick", this._doubleClickHandler)
        }

        const todoFilterElement = this.getElementByTag('filter-todo');
        if(todoFilterElement){
            todoFilterElement.addEventListener("focusout", this._filterHandler);
        }

        const sortElement = this.getElementByTag('sort-buttons');
        if(sortElement) {
            sortElement.addEventListener('click', this._toggleSortHandler);
        }

        const importElement = this.getElementByTag('import-button');
        if(importElement) {
            importElement.addEventListener('click', this._importHandler);
        }

        const importElement2 = this.getElementByTag('file-input');
        if(importElement2) {
            importElement2.addEventListener('change', this._uploadHandler)
        }

        const exportElement = this.getElementByTag('export-button');
        if(exportElement) {
            exportElement.addEventListener('click', this._exportHandler);
        }
    }

    _filterHandler = (event) => {
        console.log(event.target.value);

        let filterString = event.target.value;
        // this.TodoManagerService.filterToDos(filterString, (err, data) => {
        //     if(err) {
        //         return this._handleError(err);
        //     }
        // });
    
        this.TodoManagerService.filterToDos((undef, records) => {
            let filteredRecords = records.filter((x) => x.input.value.includes(filterString));
            this.setItemsClean(filteredRecords);
        });
    };

    populateItemList(callback) {
        this.TodoManagerService.listToDos(callback);
    }

    _addNewListItem() {
        // Get the identifier as the index of the new element
        let fieldIdentifier = this.model.items.length + 1;

        let newItem = {
            checkbox: {
                name: 'todo-checkbox-' + fieldIdentifier,
                checked: false
            },
            input: {
                name: 'todo-input-' + fieldIdentifier,
                value: this.model.item.value,
                readOnly: true
            },
            delete: {
                name: 'todo-delete-' + fieldIdentifier
            },
            date : new Date().toLocaleString()
        };

        this.TodoManagerService.createToDo(newItem, (err, data) => {
            if (err) {
                return this._handleError(err);
            }

            // Bring the path and the seed to the newItem object
            newItem = {
                ...newItem,
                ...data
            };

            // Appended to the "items" array
            this.model.items.push(newItem);

            // Clear the "item" view model
            this.model.item.value = '';

            this.setItemsClean(this.model.items);
        });
    }

    stringIsBlank(str) {
        return (!str || /^\s*$/.test(str));
    }

    _mainInputBlurHandler = (event) => {
        // We shouldn't add a blank element in the list
        if (!this.stringIsBlank(event.target.value)) {
            this._addNewListItem();
        }
    }

    _blurHandler = (event) => {
        // Change the readOnly property to true and save the changes of the field
        let currentToDo = this.changeReadOnlyPropertyFromEventItem(event, true);
        this.editListItem(currentToDo);
    }

    _doubleClickHandler = (event) => {
        // Change the readOnly property in false so we can edit the field
        this.changeReadOnlyPropertyFromEventItem(event, false);
    }

    _uploadHandler = (event) => {
        let uploadedFile = event.target.files[0];
        let fileReader = new FileReader();
        fileReader.onload = event => {
            let copyItems = this.model.items;
            copyItems.forEach(item => {
                this.deleteListItem(item);
            });
            copyItems = JSON.parse(event.target.result);
            copyItems.forEach(item => this.editListItem(item));

            let recFun = (items, index) => {
                if(items.length - 1 > index)
                    this.TodoManagerService.createToDo(items[index], (err, data) => {
                        this.model.items.push(items[index]);
                        recFun(items, index + 1);
                    });
                else
                    this.populateItemList((err, data) => {
                        if (err) {
                            return this._handleError(err);
                        } else {
                            this.setItemsClean(data);
                        }
                    });
            }
            setTimeout(recFun(copyItems), 5000);


        };
        fileReader.onerror = error => console.log(error);
        fileReader.readAsText(uploadedFile)
    }

    _importHandler = (event) => {
        let fileInputElement = document.getElementById('file-input');
        fileInputElement.click();
    }

    _exportHandler = (event) => {
        let jsonData = JSON.stringify(this.model.items);
        let dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(jsonData);

        let exportFileDefaultName = 'toDos.json';
    
        let linkElement = document.createElement('a');
        linkElement.setAttribute('href', dataUri);
        linkElement.setAttribute('download', exportFileDefaultName);
        linkElement.click();
    }

    _toggleSortHandler = (event) => {
        let buttonEl = undefined;

        if(event.path.some(x => x.classList && x.classList.contains('sort-button'))) {
            buttonEl = event.path.find(x => x.classList.contains('sort-button'));
        } 
        else if(event.target.classList.contains('sort-button')) {
            buttonEl = event.target;
        }
        else {
            return;
        }

        let neutral = buttonEl.querySelector('.sort-none-label');
        let ascending = buttonEl.querySelector('.sort-up-label');
        let descending = buttonEl.querySelector('.sort-down-label');
        let hiddenClass = 'hidden-sort-direction';
        let sortDirection = 0;

        if(!neutral.classList.contains(hiddenClass)) {
            neutral.classList.toggle(hiddenClass);
            descending.classList.toggle(hiddenClass);
            sortDirection = 1;
        }
        else if(!descending.classList.contains(hiddenClass)) {
            descending.classList.toggle(hiddenClass);
            ascending.classList.toggle(hiddenClass);
            sortDirection = -1;
        }
        else {
            ascending.classList.toggle(hiddenClass);
            neutral.classList.toggle(hiddenClass);
        }

        if(buttonEl.id == 'sort-alphabetically')
            this.sortAlphabetically = sortDirection;
        else if (buttonEl.id == 'sort-checkbox')
            this.sortCheckbox = sortDirection;

        this.populateItemList((err, data) => {
            if (err) {
                return this._handleError(err);
            } else {
                this.setItemsClean(data);
            }
        });
    }

    changeReadOnlyPropertyFromEventItem = (event, readOnly) => {
        let elementName = event.target.name;
        if (!elementName || !elementName.includes('todo-input')) {
            return;
        }

        let items = this.model.items
        let itemIndex = items.findIndex((todo) => todo.input.name === elementName)
        items[itemIndex].input = {
            ...items[itemIndex].input,
            readOnly: readOnly
        }
        this.setItemsClean(items);
        return items[itemIndex];
    }

    _ToDoClickHandler = (event) => {
        let elementName = event.target.name;      
        if (!elementName || (!elementName.includes('todo-checkbox') && !elementName.includes('todo-delete'))) {
            return;
        }

        let items = this.model.items;

        if(elementName.includes('todo-checkbox')){
            let itemIndex = items.findIndex((todo) => todo.checkbox.name === event.target.name)
            items[itemIndex].checkbox = {
                ...items[itemIndex].checkbox,
                checked: !items[itemIndex].checkbox.checked,
            }
            this.editListItem(items[itemIndex]);
            setTimeout(this.setItemsClean(items), 3000);
        }

        console.log(event);
        if(elementName.includes('todo-delete'))
        {
            this.setItemsClean(items);
            let itemIndex = items.findIndex((todo) => {
                return todo.delete.name === event.target.name
            });

            this.deleteListItem(items[itemIndex]);
            setTimeout(this.setItemsClean(items), 3000);
        }
    }

    todoIsValid(todo) {
        // Check if the todo element is valid or not
        return !(!todo || !todo.input || !todo.checkbox);
    }

    editListItem(todo) {
        if (!this.todoIsValid(todo)) {
            return;
        }
        this.TodoManagerService.editToDo(todo, (err, data) => {
            if(err) {
                return this._handleError(err);
            }
        });
    }

    setItemsClean = (newItems) => {
        if (newItems) {
            this.model.items = JSON.parse(JSON.stringify(newItems));
            this.model.items = this.model.items.filter((item) => !item.__deleted).sort((a,b) => new Date(a.date) - new Date(b.date));
            
            this.model.items.forEach(x => {
                let indexUI = this.model.items.indexOf(x);
                x.input.name = x.input.name.slice(0, -1) + indexUI;
                x.delete.name = x.delete.name.slice(0, -1) + indexUI;
                x.checkbox.name = x.checkbox.name.slice(0, -1) + indexUI;
            });

            console.log(this.model.items);

            if(this.sortCheckbox == 1)
            {
                if(this.sortAlphabetically == 1)
                    this.model.items = this.model.items.sort((a, b) => 
                        ((a.checkbox.checked === b.checkbox.checked)? 0 : a.checkbox.checked? -1 : 1) || a.input.value.localeCompare(b.input.value));
                else if(this.sortAlphabetically == -1)
                    this.model.items = this.model.items.sort((a, b) => 
                        ((a.checkbox.checked === b.checkbox.checked)? 0 : a.checkbox.checked? -1 : 1) || b.input.value.localeCompare(a.input.value));
                else 
                    this.model.items = this.model.items.sort((a, b) => (a.checkbox.checked === b.checkbox.checked)? 0 : a.checkbox.checked? -1 : 1);
            }
            else if(this.sortCheckbox == -1)
            {
                if(this.sortAlphabetically == 1)
                    this.model.items = this.model.items.sort((a, b) => 
                        ((b.checkbox.checked === a.checkbox.checked)? 0 : b.checkbox.checked? -1 : 1) || a.input.value.localeCompare(b.input.value));
                else if(this.sortAlphabetically == -1)
                    this.model.items = this.model.items.sort((a, b) => 
                        ((b.checkbox.checked === a.checkbox.checked)? 0 : b.checkbox.checked? -1 : 1) || b.input.value.localeCompare(a.input.value));
                else 
                    this.model.items = this.model.items.sort((a, b) => (b.checkbox.checked === a.checkbox.checked)? 0 : b.checkbox.checked? -1 : 1);
            }
            else if(this.sortCheckbox == 0)
            {
                if(this.sortAlphabetically == 1)
                    this.model.items = this.model.items.sort((a, b) => 
                        a.input.value.localeCompare(b.input.value));
                else if(this.sortAlphabetically == -1)
                    this.model.items = this.model.items.sort((a, b) => 
                        b.input.value.localeCompare(a.input.value));
            }
        } 
        else 
        {
            this.model.items = [];
        }

        setTimeout(() => {}, 1000);
    }

    deleteListItem(todo) {
        this.TodoManagerService.removeToDo(todo, (err, data) => {
            if(err) {
                return this._handleError(err);
            } else {
                this.TodoManagerService.listToDos((err, data) => {      
                    if (err) {
                        return this._handleError(err);
                    } else {
                        this.setItemsClean(data);
                    }
                });
            }
        });
    }

    _handleError = (err) => {
        const message = "Caught this:" + err.message + ". Do you want to try again?"
        this.showErrorModal(
            message, // An error or a string, it's your choice
            'Oh no, an error..',
            () => {
                console.log("Let's try a refresh");
                window.location.reload();
            },
            () => {
                console.log('You choose not to refresh! Good luck...');
            },
            {
                disableExpanding: true,
                cancelButtonText: 'No',
                confirmButtonText: 'Yes',
                id: 'error-modal'
            }
        );
    }
}
