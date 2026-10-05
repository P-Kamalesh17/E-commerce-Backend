# ECommerce API Controllers

Base route for all controllers: `api/[controller]`

## CartController (`api/Cart`)

| Method | Route | Action | Parameters |
|---|---|---|---|
| GET | `get-cart/{userId}` | `GetCart` | `string userId` |
| GET | `get-cart-summary/{userId}` | `GetCartSummary` | `string userId` |
| POST | `add-item` | `AddItem` | `[FromBody] AddCartItemVM addCartItemVM` |
| PUT | `update-item/{cartItemId:int}` | `UpdateItem` | `int cartItemId`, `[FromBody] UpdateCartItemVM updateCartItemVM` |
| DELETE | `remove-item/{cartItemId:int}` | `RemoveItem` | `int cartItemId` |
| DELETE | `clear-cart/{userId}` | `ClearCart` | `string userId` |
| POST | `checkout` | `Checkout` | `[FromBody] CheckoutVM checkoutVM` |

## ProductController (`api/Product`)

| Method | Route | Action | Parameters |
|---|---|---|---|
| GET | `get-all-products` | `GetAllProducts` | — |
| GET | `get-product-by-id/{id:int}` | `GetProductById` | `int id` |
| GET | `get-by-category/{categoryId:int}` | `GetProductsByCategoryId` | `int categoryId` |
| POST | `create-product` | `CreateProduct` | `[FromBody] CreateProductVM createProductVM` |
| PUT | `update-product-by-id/{id:int}` | `UpdateProductById` | `int id`, `[FromBody] CreateProductVM createProductVM` |
| DELETE | `delete-product-by-id/{id:int}` | `DeleteProductById` | `int id` |
| GET | `filter` | `FilterProducts` | `[FromQuery] decimal? minPrice`, `[FromQuery] decimal? maxPrice`, `[FromQuery] int? categoryId` |

## CategoryController (`api/Category`)

| Method | Route | Action | Parameters |
|---|---|---|---|
| GET | `get-all-category` | `GetAllCategory` | — |
| GET | `get-category-by-id/{categoryid}` | `GetCategoryById` | `int categoryid` |
| POST | `create-category` | `CreateCategory` | `[FromBody] CategoryVM categoryVM` |
| PUT | `update-category/{id}` | `UpdateCategory` | `int id`, `[FromBody] UpdateCategoryVM categoryVM` |
| DELETE | `delete-category-by-id/{id}` | `DeleteCategoryById` | `int id` |

## OrderController (`api/Order`)

| Method | Route | Action | Parameters |
|---|---|---|---|
| GET | `get-all-orders` | `GetAllOrders` | `[FromQuery] OrderStatus? orderStatus` |
| GET | `get-order-by-id/{id}` | `GetOrderById` | `int id` |
| GET | `get-orders-by-userid/{userid}` | `GetOrdersByUserId` | `string userid` |
| GET | `get-orders-by-status/{status}` | `GetOrdersByStatus` | `[FromQuery] OrderStatus status` |
| GET | `get-pending-orders` | `GetPendingOrders` | — |
| POST | `create-order` | `CreateOrder` | `[FromBody] CreateOrderVM createOrderVM` |
| PUT | `update-order/{id}` | `UpdateOrder` | `int id`, `[FromBody] UpdateOrderVM updateOrderVM` |
| PATCH | `update-order-status/{id}` | `UpdateOrderStatus` | `int id`, `[FromBody] UpdateOrderStatusVM updateOrderStatusVM` |
| PATCH | `cancel-order/{id}` | `CancelOrder` | `int id` |
| DELETE | `delete-order/{id}` | `DeleteOrder` | `int id` |

## OrderItemController (`api/OrderItem`)

| Method | Route | Action | Parameters |
|---|---|---|---|
| GET | `get-order-items/{orderId}` | `GetOrderItems` | `int orderId` |
| GET | `get-order-item/{id}` | `GetOrderItem` | `int id` |

## PaymentController (`api/Payment`)

| Method | Route | Action | Parameters |
|---|---|---|---|
| GET | `get-all-payments` | `GetAllPayments` | — |
| GET | `get-payment/{id}` | `GetPayment` | `int id` |
| GET | `get-payment-by-order/{orderId}` | `GetPaymentByOrder` | `int orderId` |
| GET | `get-user-payments/{userId}` | `GetUserPayments` | `string userId` |
| POST | `create-payment` | `CreatePayment` | `[FromBody] CreatePaymentVM createPaymentVM` |
| POST | `confirm-payment/{id}` | `ConfirmPayment` | `int id` |
| POST | `fail-payment/{id}` | `FailPayment` | `int id` |
| POST | `refund-payment/{id}` | `RefundPayment` | `int id` |
| PATCH | `update-payment-status/{id}` | `UpdatePaymentStatus` | `int id`, `[FromBody] UpdatePaymentStatusVM updatePaymentStatusVM` |
