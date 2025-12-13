var cart = {
    init: function () {
        cart.regEvents();
    },
    regEvents: function () {
        $('#btnContinue').off('click').on('click', function () {
            window.location.href = "/";
        });
        $('#btnPayment').off('click').on('click', function () {
            window.location.href = "/thanh-toan";
        });
        $('.txtQuantity').change(function () {
            var listProduct = $('.txtQuantity');
            var cartList = [];
            $.each(listProduct, function (i, item) {
                cartList.push({
                    Quantity: $(item).val(),
                    Product: {
                        ProductId: $(item).data('id')
                    }
                });
            });

            $.ajax({
                url: '/Cart/Update',
                data: { cartModel:JSON.stringify(cartList) },
                dataType: 'json',
                type: 'POST',
                success: function (res) {
                    if (res.status == true) {
                        window.location.href = "/gio-hang";
                    }
                }
            })
        });

        $('#btnDeleteAll').off('click').on('click', function () {


            $.ajax({
                url: '/Cart/DeleteAll',
                dataType: 'json',
                type: 'POST',
                success: function (res) {
                    if (res.status == true) {
                        window.location.href = "/gio-hang";
                    }
                }
            })
        });

        $('.btn-delete').off('click').on('click', function (e) {
            e.preventDefault();
            $.ajax({
                data: { id: $(this).data('id') },
                url: '/Cart/Delete',
                dataType: 'json',
                type: 'POST',
                success: function (res) {
                    if (res.status == true) {
                        window.location.href = "/gio-hang";
                    }
                }
            })
        });
       
    }
}
cart.init();
function deleteItem(productId) {
    $.ajax({
        url: '/Cart/Delete',  // Đường dẫn đến phương thức Delete trong CartController
        type: 'POST',  // Gửi yêu cầu POST
        data: { id: productId },  // Dữ liệu gửi đi, bao gồm ProductId cần xóa
        success: function (response) {  // Hàm gọi lại khi yêu cầu thành công
            if (response.status) {
                alert("Sản phẩm đã được xóa!");  // Thông báo nếu xóa thành công
                location.reload();  // Tải lại trang để cập nhật giỏ hàng
            } else {
                alert("Có lỗi xảy ra khi xóa sản phẩm.");  // Thông báo nếu có lỗi
            }
        },
        error: function () {  // Hàm gọi lại khi có lỗi trong yêu cầu AJAX
            alert("Không thể thực hiện xóa sản phẩm. Vui lòng thử lại.");
        }
    });
}