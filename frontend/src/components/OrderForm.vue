<template>
  <form @submit.prevent="send">
    <div>
      <label>Customer ID</label>
      <input v-model="customerId" />
    </div>
    <div>
      <label>SKU</label>
      <input v-model="sku" />
    </div>
    <div>
      <label>Quantity</label>
      <input type="number" v-model.number="qty" />
    </div>
    <button type="submit">Submit</button>
    <p v-if="jobId">Job ID: {{ jobId }}</p>
  </form>
</template>

<script setup>
import { ref } from 'vue';
import { submitOrder } from '../dispatcherClient';

const customerId = ref('');
const sku = ref('');
const qty = ref(1);
const jobId = ref('');

function generateUuid() {
  return (crypto.randomUUID && crypto.randomUUID()) || Math.random().toString(36).substring(2);
}

async function send() {
  const order = {
    orderId: { value: generateUuid() },
    customerId: customerId.value,
    items: [
      {
        sku: sku.value,
        qty: { value: qty.value },
        unitPrice: { amount: 0, currency: 'EUR' }
      }
    ]
  };
  try {
    const resp = await submitOrder(order);
    jobId.value = resp.jobId?.value || '';
  } catch (err) {
    console.error(err);
  }
}
</script>
