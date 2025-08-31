<template>
  <div class="container">
    <OrderForm @submitted="addOrder" />
    <OrdersGrid :orders="orders" />
  </div>
</template>

<script setup>
import { ref } from 'vue';
import OrderForm from './components/OrderForm.vue';
import OrdersGrid from './components/OrdersGrid.vue';

const orders = ref([]);

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
