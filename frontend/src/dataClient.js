import protobuf from 'protobufjs';

const root = new protobuf.Root();
root.resolvePath = (origin, target) => {
  if (target.startsWith('contracts/')) {
    return `/${target}`;
  }
  return protobuf.util.path.resolve(origin, target);
};

const rootPromise = root.load('/contracts/api/v1/data.proto');

function frameRequest(buf) {
  const out = new Uint8Array(5 + buf.length);
  out[0] = 0;
  out[1] = (buf.length >>> 24) & 0xff;
  out[2] = (buf.length >>> 16) & 0xff;
  out[3] = (buf.length >>> 8) & 0xff;
  out[4] = buf.length & 0xff;
  out.set(buf, 5);
  return out;
}

function parseResponse(buf) {
  const len = (buf[1] << 24) | (buf[2] << 16) | (buf[3] << 8) | buf[4];
  return buf.slice(5, 5 + len);
}

export async function listOrders() {
  const root = await rootPromise;
  const ListOrdersRequest = root.lookupType('poc.micro.ordering.api.v1.ListOrdersRequest');
  const ListOrdersResponse = root.lookupType('poc.micro.ordering.api.v1.ListOrdersResponse');
  const message = ListOrdersRequest.create({});
  const body = frameRequest(ListOrdersRequest.encode(message).finish());

  const res = await fetch('http://localhost:8080/poc.micro.ordering.api.v1.Data/ListOrders', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/grpc-web+proto',
      'X-Grpc-Web': '1'
    },
    body
  });

  const buffer = new Uint8Array(await res.arrayBuffer());
  return ListOrdersResponse.decode(parseResponse(buffer));
}
