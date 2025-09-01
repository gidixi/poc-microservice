<template>
  <div class="container">
    <OrderForm @submitted="addOrder" />
    <OrdersGrid :orders="orders" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import OrderForm from './components/OrderForm.vue';
import OrdersGrid from './components/OrdersGrid.vue';
import { fetchOrders } from './dispatcherClient';

const orders = ref([]);

function addOrder(order) {
  orders.value.push(order);
}

onMounted(async () => {
  try {
    const res = await fetchOrders();
    orders.value = res.orders.map(o => ({ ...o.order, jobId: o.order.orderId?.value }));
  } catch (err) {
    console.error(err);
  }
});
</script>

<style scoped>
.container {
  font-family: Arial, sans-serif;
  padding: 20px;
}
</style>
