function OnSuccess(data) {
    var res = $('#results');
    res.empty();

    var responce = data;
    if (responce.name == "AttestationComleted") {
        $('#dis_' + responce.id).prop("disabled", true);
        $('#saveResult').prop("disabled", true);
        $('#AttestationComplited').prop("hidden", false);
    }
    if (responce.name == "AddSucces") {
        $('#dis_' + responce.id).prop("disabled", true);
    }
    if (responce.id == 0 && responce.name == "StudentNotFound") {
        res.append('<h4>Вы не выбрали студента! Выберите студента из списка!</h4>');
    }
    this.reset();
};