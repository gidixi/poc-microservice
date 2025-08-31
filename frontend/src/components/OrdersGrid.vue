<template>
    <table v-if="orders.length" class="orders-grid">
      <thead>
        <tr>
          <th>Order ID</th>
          <th>Customer ID</th>
          <th>SKU</th>
          <th>Quantity</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="order in orders" :key="order.orderId.value">
          <td>{{ order.orderId.value }}</td>
          <td>{{ order.customerId }}</td>
          <td>{{ order.items[0].sku }}</td>
          <td>{{ order.items[0].qty.value }}</td>
        </tr>
      </tbody>
    </table>
    <p v-else>No orders yet.</p>
  </template>

<script setup>
import { defineProps } from 'vue';

defineProps({
  orders: {
    type: Array,
    default: () => []
  }
});
</script>

<style scoped>
.orders-grid {
  border-collapse: collapse;
  width: 100%;
  margin-top: 20px;
}
.orders-grid th, .orders-grid td {
  border: 1px solid #ddd;
  padding: 8px;
}
.orders-grid th {
  background-color: #f2f2f2;
  text-align: left;
}
</style>
