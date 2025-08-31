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
import { listOrders } from './dataClient';

const orders = ref([]);

onMounted(async () => {
  try {
    const resp = await listOrders();
    orders.value = resp.orders.map(o => o.order);
  } catch (err) {
    console.error(err);
  }
});

function addOrder(order) {
  orders.value.push(order);
}
</script>

<style scoped>
.container {
  font-family: Arial, sans-serif;
  padding: 20px;
}
</style>
