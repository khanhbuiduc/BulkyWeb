var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else if (url.includes("completed")) {
        loadDataTable("completed");
    }
    else if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else if (url.includes("approved")) {
        loadDataTable("approved");
    }
    else {
        loadDataTable("all");
    }
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "destroy": true,
        "ajax": {
            "url": "/Admin/Order/GetAll?status=" + status,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "20%" },
            { 
                "data": "orderStatus", 
                "width": "15%",
                "render": function (data) {
                    if (data == "Approved") {
                        return '<span class="badge bg-success"><i class="bi bi-check-circle"></i> ' + data + '</span>';
                    } else if (data == "Pending") {
                        return '<span class="badge bg-warning text-dark"><i class="bi bi-clock"></i> ' + data + '</span>';
                    } else if (data == "Processing") {
                        return '<span class="badge bg-info"><i class="bi bi-arrow-repeat"></i> ' + data + '</span>';
                    } else if (data == "Shipped") {
                        return '<span class="badge bg-primary"><i class="bi bi-truck"></i> ' + data + '</span>';
                    } else if (data == "Cancelled") {
                        return '<span class="badge bg-danger"><i class="bi bi-x-circle"></i> ' + data + '</span>';
                    } else if (data == "Refunded") {
                        return '<span class="badge bg-secondary"><i class="bi bi-arrow-counterclockwise"></i> ' + data + '</span>';
                    }
                    return data;
                }
            },
            { 
                "data": "orderTotal", 
                "width": "10%", 
                "render": function (data) { 
                    return '$' + data.toFixed(2); 
                } 
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="w-100 btn-group" role="group">
                            <a href="/Admin/Order/Details?orderId=${data}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Details
                            </a>
                        </div>
                    `;
                },
                "width": "20%"
            }
        ]
    });

    // Update active nav link
    $('.nav-link').removeClass('active');
    $('a[onclick="loadDataTable(\'' + status + '\')"]').addClass('active');
}
