var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/user/getall' },
        "columns": [
            { data: 'name', "width": "15%" },
            { data: 'email', "width": "20%" },
            { data: 'phoneNumber', "width": "10%" },
            { data: 'company.name', "width": "15%" },
            { data: 'role', "width": "10%" },
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        return `
                        <div class="w-100 btn-group" role="group">
                            <a href="/admin/user/upsert?id=${data.id}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onClick=LockUnlock('${data.id}') class="btn btn-danger mx-2">
                                <i class="bi bi-lock-fill"></i> Unlock
                            </a>
                        </div>
                        `
                    }
                    else {
                        return `
                        <div class="w-100 btn-group" role="group">
                            <a href="/admin/user/upsert?id=${data.id}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>
                            <a onClick=LockUnlock('${data.id}') class="btn btn-success mx-2">
                                <i class="bi bi-unlock-fill"></i> Lock
                            </a>
                        </div>
                        `
                    }
                },
                "width": "30%"
            }
        ]
    });
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }
    });
}
