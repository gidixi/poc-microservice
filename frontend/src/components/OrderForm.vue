<template>
  <form class="order-form" @submit.prevent="send">
    <div class="form-row">
      <label>Customer ID</label>
      <input v-model="customerId" />
    </div>
    <div class="form-row">
      <label>SKU</label>
      <input v-model="sku" />
    </div>
    <div class="form-row">
      <label>Quantity</label>
      <input type="number" v-model.number="qty" />
    </div>
    <button class="submit-btn" type="submit">Submit</button>
    <p v-if="jobId">Job ID: {{ jobId }}</p>
  </form>
</template>

<script setup>
import { ref, defineEmits } from 'vue';
import { submitOrder } from '../dispatcherClient';

const emit = defineEmits(['submitted']);

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
    emit('submitted', { ...order, jobId: jobId.value });
    customerId.value = '';
    sku.value = '';
    qty.value = 1;
  } catch (err) {
    console.error(err);
  }
}
</script>

<style scoped>
.order-form {
  display: flex;
  flex-direction: column;
  gap: 10px;
  background: #f9f9f9;
  padding: 20px;
  border-radius: 8px;
  max-width: 400px;
}
.form-row {
  display: flex;
  flex-direction: column;
}
.submit-btn {
  align-self: flex-start;
}
</style>
