<script lang="ts">
    import { onMount } from "svelte";

    import { isLoggedIn } from "$lib/Handlers/accountHandler";

    onMount(async () => {
        if(await isLoggedIn()) {
            window.location.href = "/chat/home";
        }
        else{
            window.location.href = "/welcome";
        }
    });
</script>
