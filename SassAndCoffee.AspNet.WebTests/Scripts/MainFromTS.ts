function conditionAlert(message : string, predicate : ()=>bool){
    window.alert(message);
}

conditionAlert("Foo", () => 4 < 3);
conditionAlert("Bar", () => 4 > 6);
conditionAlert("TypeScript!", () =>  1 < 2);


