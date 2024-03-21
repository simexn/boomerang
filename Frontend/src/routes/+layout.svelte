<script lang="ts">
    import '$lib/css/mainstyles.css'
    import { onMount } from 'svelte';
    import {fetchChats} from '$lib/Handlers/groupHandler'
    import {getToken} from '$lib/Handlers/authHandler'
</script>
    

<div class="container-fluid">
    <nav class="navbar navbar-expand-lg navbar-light">
        <a class="navbar-brand" href="/">Boomerangr</a>
    </nav>  
    <main>
        <slot/>
    </main>
</div>


<style>
    .container-fluid {
        box-sizing: border-box;
        padding:0;
        display: flex;
        flex-direction: column;
        height: 100vh;
        overflow: hidden;
    }
        main {
            height:100%;
            display: flex;
            flex-direction: row;
            overflow: hidden;
        }
            nav{
                background-color: var(--prim);
            }
            a{
                color: var(--prim-fg) !important;
                margin-left: 2rem;
            }
</style>






