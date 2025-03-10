﻿/*------------------------------------------------------------------------------------*/
/*-------------------------Table data for Booking section-----------------------------*/
/*------------------------------------------------------------------------------------*/
var dataTable;

$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const status = urlParams.get('status');
    loadDataTable(status);
});

function loadDataTable(status) {
    if ($.fn.DataTable.isDataTable('#tblBookings')) {
        $('#tblBookings').DataTable().destroy(); // Xóa instance cũ nếu có
    }

    $('#tblBookings').DataTable({
        serverSide: true, // Tắt server-side processing
        processing: true,
        ajax: {
            url: '/booking/getallbookingsdata?status=' + status,
            type: 'GET',
            dataType: 'json',
            dataSrc: function (json) {
                console.log("🔍 API Response:", json);
                return json.data || []; // Tránh lỗi nếu data undefined
            },
            error: function (xhr, status, error) {
                console.error("❌ AJAX Error:", xhr.responseText);
            }
        },
        columns: [
            { data: 'id', width: "5%" },
            { data: 'name', width: "15%" },
            { data: 'phone', width: "10%" },
            { data: 'email', width: "15%" },
            { data: 'status', width: "10%" },
            { data: 'checkInDate', width: "10%" },
            { data: 'nights', width: "5%" },
            { data: 'totalCost', render: $.fn.dataTable.render.number(',', '.', 2, '$'), width: "10%"},
            {
                data: 'id',
                "className": "text-center",
                "render": function (data) {
                    return `<div class="w-75 btn-group">
                        <a href="/booking/bookingDetails?bookingId=${data}" class="btn btn-outline-warning mx-2">
                            <i class="bi bi-pencil-square"></i> Details
                        </a>
                    </div>`
                }
            }
        ],
        order: [[0, 'asc']], // Sắp xếp theo cột ID
        responsive: true,
        autoWidth: false
    });
}