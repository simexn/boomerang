<script>
    import { onMount } from "svelte";

    let data = ""

    onMount(async ()=>{
        const response = await fetch('https://localhost:1710/account/info', {
        method: 'GET'
        });

        data = await response.text();
        console.log(data);
    });


</script>
<p>{data}</p>