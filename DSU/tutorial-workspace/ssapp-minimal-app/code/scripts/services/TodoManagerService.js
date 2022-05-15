const TO_DO_TABLE = "todos";

class TodoManagerService {

    constructor(enclaveDB) {
        this.enclave = enclaveDB;
    }

    createToDo(todo, callback) {
        this.enclave.insertRecord(TO_DO_TABLE, todo.input.name, todo, (err, res) => {
            if(err){    
                if(err.message.includes('existing')) {
                    // console.log(err);
                    console.log('will update existing record');
                    // console.log(todo);

                    this.enclave.getAllRecords(TO_DO_TABLE, (err2, values) => {
                        let index = values.findIndex(x => x.__deleted);
                        if(index >= 0) {
                            this.enclave.updateRecord(TO_DO_TABLE, values[index].pk, todo, (err3, res2) => {
                                this.enclave.getAllRecords(TO_DO_TABLE, (err4, values2) => {
                                    console.log(values2);
                                    callback(err4);
                                });
                            });
                        }
                        else {
                            console.log('-------');
                            console.log('what should i do here?');
                            console.log('-----------');
                        }
                    });

                    // this.enclave.updateRecord(TO_DO_TABLE, todo.pk ? todo.pk : todo.input.name, todo, (err2, res) => {
                    //     this.enclave.getAllRecords(TO_DO_TABLE, (err3, values) => {
                    //         console.log(values);
                    //         callback(err3);
                    //     });
                    // });
                }
                else{
                    callback(err);
                }               
            } 
            else {
                console.log('created new record');
                callback(err, res);
            }
        });
    }

    removeToDo(todo, callback) {
        this.enclave.deleteRecord(TO_DO_TABLE, todo.pk, (err, data) => {
            if(err) {
                return callback(err, data);
            }
            
            this.enclave.getAllRecords(TO_DO_TABLE, (undef, values) => {
                console.log('before swap');
                console.log(values);
    
                let index = values.findIndex(x => x.pk == todo.pk);
                let secondIndex = values.slice().reverse().findIndex(x => !x.__deleted);
                secondIndex = secondIndex >= 0 ? values.length - 1 - secondIndex : values.length - 1;
    
                console.log('index1 = ' + index + ', index2 = ' + secondIndex);
    
                if(index != secondIndex && index >= 0 && secondIndex >= 0) {
                    console.log(values[index]);
                    console.log(values[secondIndex]);

                    this.enclave.updateRecord(TO_DO_TABLE, values[index].pk, values[secondIndex], (err, newRec) => {
                        console.log(err);
                        console.log(err);

                        this.enclave.getAllRecords(TO_DO_TABLE, (undef, values2) => {
                            console.log('-----');
                            console.log(values2);
                            console.log('-----');
                        });

                        this.enclave.updateRecord(TO_DO_TABLE, values[secondIndex].pk, values[index], () => {
                        
                            this.enclave.getAllRecords(TO_DO_TABLE, (undef, values3) => {
                                console.log(values3);
                            });
                        });
                    });
                }
            });

            callback(err, data);
        });
    }

    editToDo(todo, callback) {
        this.enclave.updateRecord(TO_DO_TABLE, todo.pk, todo, callback);
    }

    listToDos(callback) {
        this.enclave.getAllRecords(TO_DO_TABLE, callback);
    }

    filterToDos(query, callback) {
        this.enclave.filter(TO_DO_TABLE, query);
    }
}

let todoManagerService;
let getTodoManagerServiceInstance = function (controllerInstance, callback) {
    if (!todoManagerService) {
        // getMainEnclaveDB
        // did not work: getBasicDB getInMemoryDB getDB
        // console.log(controllerInstance);
        controllerInstance.getMainEnclaveDB((err, enclave) => {
            if (err) {
                console.log('Could not get main enclave ', err);
                return callback(err);
            }
            todoManagerService = new TodoManagerService(enclave);
            return callback(todoManagerService)
        })

    } else {
        return callback(todoManagerService);
    }
}

export {
    getTodoManagerServiceInstance
};
